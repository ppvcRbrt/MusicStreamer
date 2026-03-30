from db import get_readonly_session
from models.dotnet_models import Track



if __name__ == '__main__':
    with get_readonly_session() as session:
        tracks = session.query(Track).limit(5).all()
        for t in tracks:
            print(f"{t.Artist.Name} - {t.Title} - {t.Album.Title}")

