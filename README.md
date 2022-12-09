# Sample Player API
Simple API for player data search

## [Clone repo](#clone-repo)

```bash
git clone https://github.com/boydc7/Samples.Players
cd Samples.Players
```

## [Docker Run](#docker-run)
To run the entire stack in Docker (inlcuding the API), simply docker compose up:

```bash
docker compose up
```

NOTE: To rebuild and force recreation of the various containers:
```bash
docker compose up --build --force-recreate --renew-anon-volumes
```
## [Docker Run Helper](#docker-run-helper)
A simple bash script is also included to help with running the stack in various states.

```bash
bash docker-up-local.sh
```

TO RESET LOCAL ENV (including a rebuild):
```bash
bash docker-up-local.sh reset
```

TO START THE API (in addition to the default of just starting dependencies):
```bash
bash docker-up-local.sh api
```

TO START THE API AND REBUILD/RESET LOCAL ENV
```bash
bash docker-up-local.sh reset api
```

TO START THE API AND REBUILD CODE ONLY (without resetting local dependencies):
```bash
bash docker-up-local.sh build api
```

## [Docker Stop](#docker-stop)

To stop the stack:
```bash
docker compose -f docker-compose.yml down
```

# [NOTES](#notes)

The API will listen on port 8082 by default.

The first time the stack is run, the API will background tasks to load demo data from a sports player API. Those are fetched and loaded into the data store and until that first load is complete, the endpoints will return nothing of note.

Once the demo data has been fetched and loaded, an additional background task will be kicked off to calculate and store some various other metrics as well. While that is running, though you can fetch information from the API, some pieces of data will be missing until complete.

The logs will output the following when the demo data fetch and load is completed:

```text
"Demo data creation complete"
```

While the background calculations are running, the log will show various information about the status of progress. When the background calculations are complete the log will show the following:

```text
"UpdateAgeAverages complete"
```

Once that is completed, the API is ready to roll.

First load on my machine takes anywhere from 15-20 seconds to complete entirely as a point of reference.


# [Endpoints](#endpoints)

You should be able to query the player data a couple of ways. 

See the PlayersController.cs file for the 3 endpoints that exist. 

GET http://localhost:8082/players/query
PARAMS:
	* sport
	* lastNameInitial
	* position
	* minAge
	* maxAge
	* age
	* skip
	* take

To get a page of players for baseball:

GET http://localhost:8082/players/query?sport=baseball

To get a page of players for baseball who pitch:

GET http://localhost:8082/players/query?sport=baseball&position=p

To get a page of players for baseball over 26:

GET http://localhost:8082/players/query?sport=baseball&minAge=26

And so on...



