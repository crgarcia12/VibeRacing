namespace VibeRacing.Server.Dto;

public record JoinLobbyDto(string RoomCode, string DisplayName);
public record SendInputDto(bool Accelerate, bool Brake, bool TurnLeft, bool TurnRight);
