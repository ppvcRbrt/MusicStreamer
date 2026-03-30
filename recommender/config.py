import os
from dotenv import load_dotenv

load_dotenv()

DATABASE_URL_RECOMMENDER = os.getenv("DATABASE_URL_RECOMMENDER")
DATABASE_URL_READONLY = os.getenv("DATABASE_URL_READONLY")
MEDIA_LOCATION = os.getenv("MEDIA_LOCATION")

if not DATABASE_URL_RECOMMENDER:
    raise ValueError("Missing DATABASE_URL_RECOMMENDER in env")
if not DATABASE_URL_READONLY:
    raise ValueError("Missing DATABASE_URL_READONLY in env")
if not MEDIA_LOCATION:
    raise ValueError("Missing MEDIA_LOCATION in env")