docker-rebuild:
	docker build -f "LitArchive.WebApi\Dockerfile" --force-rm -t lit-archive ./../../..

docker-build:
	docker build -f "LitArchive.WebApi\Dockerfile" -t lit-archive ./../../..

docker-save:
	docker save --output "\\litnas\shared\docker-images\lit-archive.tar" lit-archive
	
deploy-docker: docker-build docker-save