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
          git log -1 --pretty=medium
        '''
      }
    }

    stage('Setup .NET SDK 8.0.414') {
      steps {
    sh '''
      set -e

      # Ensure we have either curl or wget
      if ! command -v curl >/dev/null 2>&1 && ! command -v wget >/dev/null 2>&1; then
        if command -v apk >/dev/null 2>&1; then
          # Alpine
          apk add --no-cache curl
        elif command -v apt-get >/dev/null 2>&1; then
          # Debian/Ubuntu
          apt-get update && apt-get install -y curl
        elif command -v yum >/dev/null 2>&1; then
          # RHEL/CentOS
          yum install -y curl
        else
          echo "No package manager (apk/apt/yum) found to install curl." >&2
          exit 1
        fi
      fi

      # Download dotnet-install.sh with curl or wget
      if command -v curl >/dev/null 2>&1; then
        curl -sSL https://dot.net/v1/dotnet-install.sh -o dotnet-install.sh
      else
        wget -qO dotnet-install.sh https://dot.net/v1/dotnet-install.sh
      fi
      chmod +x dotnet-install.sh

      # Install requested SDK to workspace-local folder
      ./dotnet-install.sh --version 8.0.414 --install-dir "$PWD/.dotnet"

      echo "## SDKs:"
      "$PWD/.dotnet/dotnet" --list-sdks
    '''
    script {
      env.PATH = "${env.WORKSPACE}/.dotnet:${env.PATH}"
      env.DOTNET_ROOT = "${env.WORKSPACE}/.dotnet"
        }
      }
    }

    stage('Restore & Build (Release)') {
      steps {
        sh 'dotnet --info'
        sh 'dotnet --list-sdks || true'
        sh 'dotnet restore'
        sh 'dotnet build --configuration Release --no-restore'
      }
    }

    stage('Test + Coverage (Release)') {
      steps {
        dir('SimpleWebApi.Test') {
          sh '''
            dotnet tool update -g trx2junit || dotnet tool install -g trx2junit
            export PATH="$HOME/.dotnet/tools:$PATH"

            # Run tests with TRX + coverage (most stable across SDKs/OS)
            dotnet vstest ../SimpleWebApi.Test/bin/Release/net8.0/SimpleWebApi.Test.dll --logger:trx || \
            dotnet test --configuration Release --no-build --logger "trx;LogFileName=results.trx" --collect:"XPlat Code Coverage"

            # Convert TRX -> JUnit for Jenkins
            trx2junit **/results.trx || true

            echo "List results for publishing:"
            ls -R
          '''
        }
      }
      post {
        always {
          junit allowEmptyResults: false, testResults: 'SimpleWebApi.Test/**/*.junit.xml'
          recordCoverage(tools: [[parser: 'COBERTURA', pattern: 'SimpleWebApi.Test/**/coverage.cobertura.xml']])
          archiveArtifacts artifacts: 'SimpleWebApi.Test/**/results.trx, SimpleWebApi.Test/**/*.junit.xml, SimpleWebApi.Test/**/coverage.cobertura.xml', onlyIfSuccessful: false
        }
      }
    }
  }
}