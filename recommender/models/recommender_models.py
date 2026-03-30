from sqlalchemy import Column, Integer, String, ForeignKey, Float
from sqlalchemy.dialects.postgresql import JSON
from sqlalchemy.orm import DeclarativeBase, relationship
from pgvector.sqlalchemy import Vector


class RecommenderBase(DeclarativeBase):
    pass

class TrackFeatures(RecommenderBase):
    __tablename__ = "track_features"
    __table_args__ = {"schema": "recommender"}

    id = Column(Integer, primary_key=True)
    dotnet_track_id = Column(Integer, nullable=False, unique=True, index=True)
    bpm = Column(Float)
    energy = Column(Float)
    danceability = Column(Float)
    key = Column(String)
    scale = Column(String)
    genres = Column(JSON)
    embedding = Column(Vector(1280))

