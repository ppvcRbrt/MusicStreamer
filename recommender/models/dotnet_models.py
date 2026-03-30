from sqlalchemy import Column, Integer, String, ForeignKey
from sqlalchemy.orm import DeclarativeBase, relationship


class DotnetBase(DeclarativeBase):
    pass

class Artist(DotnetBase):
    __tablename__ = "Artists"
    Id = Column(Integer, primary_key=True)
    Name = Column(String)
    ImageUrl = Column(String)

class Album(DotnetBase):
    __tablename__ = "Albums"
    Id = Column(Integer, primary_key=True)
    Title = Column(String)
    Genre = Column(String)
    ImageUrl = Column(String)
    Year = Column(Integer)

class Track(DotnetBase):
    __tablename__ = "Tracks"
    Id = Column(Integer, primary_key=True)
    Title = Column(String)
    Genre = Column(String)
    AlbumId = Column(Integer, ForeignKey("Albums.Id"))
    ArtistId = Column(Integer, ForeignKey("Artists.Id"))
    ImageUrl = Column(String)
    Duration = Column(Integer)
    FilePath = Column(String)
    TrackNumber = Column(Integer)
    Format = Column(String)

    Album = relationship("Album")
    Artist = relationship("Artist")
