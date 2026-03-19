# DustRacing2D - MiniCore Engine Specification

## 1. DIRECTORY STRUCTURE

```
MiniCore/src/
├── Physics/
│   ├── MCPhysicsComponent   — physics integration per object
│   ├── MCCollisionDetector  — collision detection
│   ├── MCImpulseGenerator   — collision response
│   ├── MCObjectGrid         — spatial partitioning (broad-phase)
│   ├── MCShape, MCCircleShape, MCRectShape  — geometry primitives
│   ├── MCContact            — contact manifold data
│   ├── MCForceGenerator     — abstract force base
│   ├── MCFrictionGenerator  — surface friction
│   ├── MCGravityGenerator   — gravity force
│   └── MCDragForceGenerator — air resistance
├── Core/
│   ├── MCObject             — base entity (Pimpl pattern)
│   ├── MCObjectComponent    — component base class
│   ├── MCWorld              — scene/physics manager (Singleton)
│   ├── MCVector2d, MCVector3d — math primitives
│   ├── MCEvent, MCCollisionEvent, MCTimerEvent — event types
│   └── MCMathUtil, MCTrigonom — math utilities
├── Graphics/
│   ├── MCSurface            — 2D texture wrapper
│   ├── MCGLObjectBase       — rendering base
│   └── MCWorldRenderer      — batched rendering
└── Asset/
    ├── MCAssetManager       — master loader (Singleton)
    ├── MCSurfaceManager     — texture loading/management
    ├── MCMeshManager        — 3D model loading (.obj)
    ├── MCSurfaceMetaData    — texture config struct
    └── MCMeshMetaData       — mesh config struct
```

---

## 2. PHYSICS ENGINE

### 2.1 Integration Method: Semi-Implicit Euler
```
velocity += (acceleration + forces/mass) * dt * damping
position += velocity * dt
angVel   += (angAccel + torque/I) * dt * damping
angle    += angVel * dt
```
- dt in milliseconds, converted internally: `step / 1000.0f`

### 2.2 Default Physics Constants
| Parameter              | Value   |
|------------------------|---------|
| Linear Damping         | 0.999   |
| Angular Damping        | 0.99    |
| Restitution            | 0.5     |
| Max Speed              | 1000.0  |
| Linear Sleep Threshold | 0.01    |
| Angular Sleep Threshold| 0.01    |
| Default Gravity        | (0, 0, -9.81) m/s² |
| Resolver Loops         | 5 iterations |
| Resolver Step          | 0.2 per iteration |

### 2.3 Collision Detection
- **Broad-phase:** MCObjectGrid — uniform spatial grid, default 128×128 cells
- **Narrow-phase:** MCCollisionDetector supports:
  - Rect vs Rect (OBB)
  - Rect vs Circle
  - Circle vs Circle
- Contact data: interpenetration depth + contact normal

### 2.4 Collision Response (MCImpulseGenerator)
```
effRestitution = 1.0 + min(resA, resB)
massScaling    = invMassA / (invMassA + invMassB)
impulse_applied = linearImpulse * effRestitution * massScaling
angularFactor  = 0.5  (hardcoded dampening)
```

### 2.5 Friction System
- Linear: `force = -v_normalized * coeffLin * gravity * mass`
- Angular: `impulse = -angVel * coeffRot * gravity * ROTATION_DECAY`
- ROTATION_DECAY = **0.01**

### 2.6 Sleep/Optimization
- Objects below both thresholds (vel < 0.01, angVel < 0.01) enter sleep
- Sleeping objects skipped during physics integration
- Sleep counter: increments each frame below threshold; triggers when counter > 1

### 2.7 Critical Collision Constants
| Constant           | Value   |
|--------------------|---------|
| Wall restitution   | 0.25f   |
| Angular calibration| 0.5f    |
| Friction threshold | 0.001f  |
| Rotation decay     | 0.01f   |

---

## 3. OBJECT/COMPONENT MODEL

### 3.1 MCObject
- Base entity class, Pimpl pattern
- Type registration system (no RTTI)
- Parent/children support (composite objects)
- Collision layer filtering: -1 = all layers; default = 0
- Boolean flags:
  - `isPhysicsObject`
  - `isTriggerObject`
  - `isParticle`
  - `bypassCollisions`

### 3.2 MCPhysicsComponent
- Attached to every MCObject
- Stores: velocity, acceleration, forces, torque, mass, moment of inertia
- Impulse-based: accumulates impulses per frame, applied at end of step
- Forces/impulses reset each frame after integration
- Velocity clamped to maxSpeed after impulses applied

### 3.3 MCWorld (Singleton)
- Manages all objects and physics integration
- Deferred removal system (prevents iterator invalidation)
- Force registry for applying generators
- Physics pipeline per step:
  ```
  MCWorld::stepTime(dt_ms)
  ├─ integratePhysics(dt_ms)
  │  ├─ forceRegistry.update()
  │  └─ per object: physicsComponent.stepTime(dt_ms)
  ├─ processRemovedObjects()
  └─ processCollisions()
     ├─ detectCollisions()        // broad + narrow phase
     ├─ iterateCurrentCollisions()
     ├─ generateImpulses()
     └─ for i in [0..resolverLoops]: resolvePositions(1/resolverLoops)
  ```

---

## 4. GRAPHICS ABSTRACTION

### 4.1 MCSurface
- 2D texture wrapper bound to OpenGL handle
- Supports per-vertex Z coordinates (tilted/3D surfaces)
- Texture coordinate remapping for atlas support
- Average color caching for render batching

### 4.2 MCSurfaceManager
- XML-based configuration (surfaces.conf)
- Image processing: color key, alpha clamp, axis mirroring
- Forces power-of-2 texture dimensions
- Supports up to 3 texture handles per surface (multitexturing)

### 4.3 Surface Configuration Format
```xml
<surface handle="car" image="car.png" w="16" h="16" xAxisMirror="1">
  <colorKey r="255" g="255" b="255"/>
  <alphaBlend src="srcAlpha" dest="oneMinusSrcAlpha"/>
</surface>
```

### 4.4 MCSurfaceMetaData Fields
```
handle, imagePath, handle2, handle3
width, height, z0, z1, z2, z3
color, colorKey, alphaBlend, alphaClamp
xAxisMirror, yAxisMirror
minFilter, magFilter, wrapS, wrapT
specularCoeff
```

---

## 5. ASSET LOADING

### 5.1 MCAssetManager (Singleton)
- Coordinates: MCSurfaceManager → MCTextureFontManager → MCMeshManager
- Sequential loading order: surfaces first, then fonts, then meshes

### 5.2 MCMeshManager
- Supports .obj format only (no .mtl materials)
- XML config maps handles to model file paths
- Per-mesh: scale and color configurable

---

## 6. EVENT SYSTEM

| Event Type         | Description                    |
|--------------------|-------------------------------|
| MCEvent            | Base event class               |
| MCCollisionEvent   | Carries collision contact data |
| MCTimerEvent       | Time-based trigger             |

- Objects receive events via `onEvent(MCEvent&)`
- No Qt signal/slot — direct virtual dispatch

---

## 7. MATH UTILITIES

- MCVector2d / MCVector3d: 2D/3D vector with dot, cross, normalize
- MCTrigonom: fast sin/cos lookup tables
- MCMathUtil: clamping, interpolation helpers
