param environmentId string
param location string
param image string
@secure()
param connectionString string
param cronExpression string = '0 0 * * 1'

resource resetJob 'Microsoft.App/jobs@2025-07-01' = {
  name: 'howsyourday-demo-reset'
  location: location
  properties: {
    environmentId: environmentId
    configuration: {
      triggerType: 'Schedule'
      replicaTimeout: 300
      replicaRetryLimit: 0
      scheduleTriggerConfig: {
        cronExpression: cronExpression
        parallelism: 1
        replicaCompletionCount: 1
      }
      secrets: [
        {
          name: 'reset-database'
          value: connectionString
        }
      ]
    }
    template: {
      containers: [
        {
          name: 'reset'
          image: image
          args: [
            '--reset-demo'
          ]
          env: [
            {
              name: 'ConnectionStrings__DefaultConnection'
              secretRef: 'reset-database'
            }
          ]
          resources: {
            cpu: json('0.25')
            memory: '0.5Gi'
          }
        }
      ]
    }
  }
}
