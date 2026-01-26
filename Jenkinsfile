pipeline {
    agent none
    stages {
        stage('CMake Debug build and unit tests') {
            agent {
                docker {
                    image 'juzzlin/qt6-24.04:latest'
                    args '--privileged -t -v $WORKSPACE:/dr2d'
                }
            }
            steps {
                sh "mkdir -p build-debug"
                sh "cd build-debug && cmake -DCMAKE_BUILD_TYPE=Debug .."
                sh "cd build-debug && cmake --build . --target all -- -j3 && ctest --output-on-failure"
            }
        }
        stage('Debian package / Ubuntu 24.04') {
            agent {
                docker {
                    image 'juzzlin/qt6-24.04:latest'
                    args '--privileged -t -v $WORKSPACE:/dr2d'
                }
            }
            steps {
                sh "mkdir -p build-deb-ubuntu-24.04"
                sh "cd build-deb-ubuntu-24.04 && cmake -D DISTRO_VERSION=Ubuntu-24.04 -D ReleaseBuild=ON -D CMAKE_BUILD_TYPE=Release .. && cmake --build . --target all -- -j3"
                sh "cd build-deb-ubuntu-24.04 && cpack -G DEB"
            }
            post {
                always {
                    archiveArtifacts artifacts: 'build-deb-ubuntu-24.04/*.deb', fingerprint: true
                }
            }
        }               
    }
}

