# HowsYourDay

HowsYourDay is a small, self-hosted journal. Pick a weather rating for how the day felt, write a few lines, and come back to see how your days have been going. Your entries stay on your server, and the sentiment analysis runs there too.

## Why I built it

A journal app is a pretty common beginner project. I wanted to see how well I could build something that small. One daily check-in, a history worth reading, and a setup someone else can run. I wanted to spend the time getting those parts right.

I added sentiment analysis because it can be hard to see a pattern when you're living one day at a time. The weather rating is how you felt. The insight gives you a short reading of what you wrote. Looking back at both can help you notice what tends to show up on lighter or heavier days.

## Run it locally

Install Docker with Docker Compose, then clone this repo. From the repo folder, copy the sample config and generate two secrets.

```sh
cp .env.example .env
openssl rand -hex 32  # POSTGRES_PASSWORD
openssl rand -hex 64  # JWT_KEY
```

Put the generated values in `.env`, then build and start the app for the first time.

```sh
docker compose up --build -d
```

Open [http://localhost:8080](http://localhost:8080) and create an account. If port 8080 is taken, change `APP_PORT` in `.env`. Compose starts PostgreSQL and applies the database migrations for you.

For everyday startup, run `docker compose up -d`. Run `docker compose up --build -d` again after pulling updates that change the app or Dockerfile.

Run `docker compose down` when you're done. Your entries will still be there the next time you start it.

## Contributing

Open an issue if you find a bug or have an idea. If you send a pull request, keep it focused and include a short note on what you changed and how you tested it.
