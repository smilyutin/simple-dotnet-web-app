pipeline {
  agent any
  environment {
    DOTNET_CLI_TELEMETRY_OPTOUT = '1'
    DOTNET_SKIP_FIRST_TIME_EXPERIENCE = '1'
  }
  stages {

    stage('Checkout') {
      steps {
        checkout scm
        sh '''
          echo "Branch:   $GIT_BRANCH"
          echo "Commit:   $GIT_COMMIT"
          echo "Author:   $GIT_AUTHOR_NAME <$GIT_AUTHOR_EMAIL>"
          echo "Message:"
          git log -1 --pretty=medium
        '''
      }
    }

    stage('Setup .NET SDK 8.0.414') {
        steps {
            sh '''
                set -e
                curl -sSL https://dot.net/v1/dotnet-install.sh -o dotnet-install.sh
                chmod +x dotnet-install.sh
                ./dotnet-install.sh --version 8.0.414 --install-dir "$PWD/.dotnet"
                echo "## Installed SDKs:"
                "$PWD/.dotnet/dotnet" --list-sdks
                '''
    // make this dotnet first in PATH for subsequent stages
        script {
            env.PATH = "${env.WORKSPACE}/.dotnet:${env.PATH}"
            env.DOTNET_ROOT = "${env.WORKSPACE}/.dotnet"
            }
        }
    }

    stage('Restore & Build (Release)') {
      steps {
        sh 'dotnet --info'
        sh 'dotnet restore'
        sh 'dotnet build --configuration Release --no-restore'
      }
    }

    stage('Test + Coverage (Release)') {
      steps {
        dir('SimpleWebApi.Test') {
          // More robust than junit logger on macOS: TRX -> JUnit conversion
          sh '''
            dotnet tool update -g trx2junit || dotnet tool install -g trx2junit

            # Run tests once with coverage + TRX output
            dotnet test --configuration Release --no-build \
              --logger "trx;LogFileName=results.trx" \
              --collect:"XPlat Code Coverage"

            # Convert TRX -> JUnit for Jenkins
            ~/.dotnet/tools/trx2junit **/results.trx || true

            echo "Workspace contents (for debugging paths):"
            pwd
            ls -R
          '''
        }
      }
      post {
        always {
          // Publish tests
          junit allowEmptyResults: false, testResults: 'SimpleWebApi.Test/**/*.junit.xml'

          // Publish coverage (Cobertura from XPlat Code Coverage)
          // The file is typically at: SimpleWebApi.Test/TestResults/**/coverage.cobertura.xml
          recordCoverage(
            tools: [[parser: 'COBERTURA', pattern: 'SimpleWebApi.Test/**/coverage.cobertura.xml']]
          )

          // (Optional) keep raw artifacts for later inspection
          archiveArtifacts artifacts: 'SimpleWebApi.Test/**/results.trx, SimpleWebApi.Test/**/*.junit.xml, SimpleWebApi.Test/**/coverage.cobertura.xml', onlyIfSuccessful: false
        }
      }
    }
  }
}