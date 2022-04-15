echo "get creds"
aws ecr get-login-password --region us-east-1 | docker login --username AWS --password-stdin 116134826460.dkr.ecr.us-east-1.amazonaws.com
echo "start build"
docker build -t fyli-core -f ./Memento/Dockerfile ./
echo "build complete"
docker tag fyli-core:latest 116134826460.dkr.ecr.us-east-1.amazonaws.com/fyli-core:latest
echo tagged
docker push 116134826460.dkr.ecr.us-east-1.amazonaws.com/fyli-core:latest
echo pushed
aws ecs update-service --region us-east-1 --cluster fyli-ecs-cluster --service fyli-service --force-new-deployment
echo "refresh done"