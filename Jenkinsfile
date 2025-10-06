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
            set -e

            # Build once so the DLL is present
            dotnet build -c Release

            # Run tests with a deterministic results folder + TRX file
            dotnet test -c Release --no-build \
            --results-directory "TestResults" \
            --logger "trx;LogFileName=results.trx" \
            --collect:"XPlat Code Coverage"

            # Install converter locally and convert TRX -> a single JUnit file
            dotnet tool install trx2junit --tool-path ./.tools || true
            ./.tools/trx2junit TestResults/results.trx -o TestResults/results.junit.xml

            echo "==== Debug listing ===="
            pwd
            find TestResults -type f -maxdepth 3 -print
        '''
        }
    }
    post {
        always {
        // Publish the one JUnit file we just created
        junit allowEmptyResults: false, testResults: 'SimpleWebApi.Test/TestResults/results.junit.xml'

        // Publish Cobertura coverage produced by XPlat collector
        recordCoverage(tools: [[parser: 'COBERTURA', pattern: 'SimpleWebApi.Test/TestResults/**/coverage.cobertura.xml']])

        // Keep all raw artifacts for inspection
        archiveArtifacts artifacts: 'SimpleWebApi.Test/TestResults/**', onlyIfSuccessful: false
            }   
      }
    }
  }
}