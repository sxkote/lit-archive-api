# Dockerize

docker build -f "LitArchive.API\Dockerfile" --force-rm -t litarchive:test .

docker save --output \\litnas\shared\docker-images\litarchive-test.tar litarchive:test

-- show docker status
docker system df

-- clear docker build-cache (to rebuild projects)
docker builder prune

