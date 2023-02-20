dotnet publish src\Lambdas\AwsImageGallery.Lambda.ListImages\ -c Release -r linux-arm64 --self-contained
cdk deploy --profile=mak