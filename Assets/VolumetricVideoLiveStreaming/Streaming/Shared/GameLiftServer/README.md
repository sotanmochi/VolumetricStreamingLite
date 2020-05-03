# Amazon GameLift

## Upload a custom server build to GameLift
AWS CLIを使ってサーバービルドをGameLiftにアップロードする
```
aws gamelift upload-build --name MJPEGStreamingServer --build-version v1.0.0 --build-root ./MJPEGStreaming-Unity/App/StreamingServerLinuxBuild --operating-system AMAZON_LINUX --region ap-northeast-1
```

## Create a fleet
AWSマネジメントコンソールでフリートを作成する

## Retrieves information about a fleet's instance
AWS CLIを使ってフリートのインスタンス情報を取得する
```
$ aws gamelift describe-instances --fleet-id "fleet-id"
{
    "Instances": [
        {
            "FleetId": "fleet-XXXXXXXXXXXXXXXXXXXXXXXX",
            "InstanceId": "i-XXXXXXXXXXXX",
            "IpAddress": "XX.XX.XX.XX",
            "DnsName": "ec2-XX-XX-XX-XX.ap-northeast-1.compute.amazonaws.com",
            "OperatingSystem": "AMAZON_LINUX",
            "Type": "c5.xlarge",
            "Status": "Active",
            "CreationTime": "2020-05-02T15:24:11.453000+09:00"
        }
    ]
}
```
