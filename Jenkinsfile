pipeline {
    agent any
    stages {
        stage('Build') { 
            steps {
                sh 'dotnet restore' 
                sh 'dotnet build --configuration Debug --no-restore'
                sh 'dotnet build --configuration Release --no-restore'
            }
        }
        stage('Test (Coverage)') {
            steps {
                sh 'dotnet test --no-build --no-restore --collect "XPlat Code Coverage"'
            }
            post {
                always {
                    recordCoverage(tools: [[parser: 'COBERTURA', pattern: '**/*.xml']], sourceDirectories: [[path: 'SimpleWebApi.Test/TestResults']])
                }
            }
        }
        stage('Test (Release)') {
  steps {
    dir('SimpleWebApi.Test') {
      sh 'dotnet test --configuration Release --no-build --logger "junit;LogFilePath=test-results.xml"'
    }
  }
  post {
    always {
      junit testResults: 'SimpleWebApi.Test/test-results.xml', allowEmptyResults: false
    }
            }
        }
    }
}
