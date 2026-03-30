CREATE SCHEMA IF NOT EXISTS recommender;
CREATE EXTENSION IF NOT EXISTS vector;

-- readonly_user: reads from public schema only
CREATE USER readonly_user WITH PASSWORD 'SuperSecretPassword';
GRANT USAGE ON SCHEMA public TO readonly_user;
GRANT SELECT ON ALL TABLES IN SCHEMA public TO readonly_user;
ALTER DEFAULT PRIVILEGES IN SCHEMA public
    GRANT SELECT ON TABLES TO readonly_user;

-- recommender_user: reads public, owns recommender schema
CREATE USER recommender_user WITH PASSWORD 'EvenMoreSecretPassword';
GRANT USAGE ON SCHEMA public TO recommender_user;
GRANT SELECT ON ALL TABLES IN SCHEMA public TO recommender_user;
ALTER DEFAULT PRIVILEGES IN SCHEMA public
    GRANT SELECT ON TABLES TO recommender_user;

GRANT USAGE, CREATE ON SCHEMA recommender TO recommender_user;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA recommender TO recommender_user;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA recommender TO recommender_user;
ALTER DEFAULT PRIVILEGES IN SCHEMA recommender
    GRANT ALL PRIVILEGES ON TABLES TO recommender_user;
ALTER DEFAULT PRIVILEGES IN SCHEMA recommender
    GRANT ALL PRIVILEGES ON SEQUENCES TO recommender_user;