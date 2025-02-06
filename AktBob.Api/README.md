# Container images

## Development
1. Build container image(s) <br>
   *Docker compose file includes a development SQL-server. Remember to publish the database project to the development SQL-server at first run.*

    `docker build --build-arg ENVIRONMENT=development -t aktbob.api:dev .`

2. Run container image (if not using docker compose)

	`docker run -p 8080:8080 aktbob.api:dev`
	
	API base url: [http://localhost:8080](http://localhost:8080)
	<br>Swagger på: [http://localhost:8080/swagger](http://localhost:8080/swagger)

---

## Deploy to production
1. Build the web project container image. We use a timestamp to identity the revision.
		
        $timestamp = Get-Date -Format "yyyyMMddHHmmss"
        docker build --build-arg ENVIRONMENT=production -t aktbobacr.azurecr.io/aktbob.api:$timestamp .

2. Azure login

	`az login`

3. AktBob ACR login

	`az acr login --name aktbobacr`

4. Push image to AktBob container registry

	`docker push aktbobacr.azurecr.io/aktbob.api:$timestamp`

5. Create new revision for the Azure Container App