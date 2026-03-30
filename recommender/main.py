from db import get_readonly_session, get_recommender_session
from models.dotnet_models import Track
from models.recommender_models import TrackFeatures
from extraction import load_audio, get_basic_features, get_embedding, get_genre
import os
os.environ["CUDA_VISIBLE_DEVICES"] = "-1"

if __name__ == '__main__':
    with get_readonly_session() as session:
        tracks = session.query(Track).all()
        for t in tracks:
            audio = load_audio(t.FilePath)
            bpm, energy, danceability, key, scale = get_basic_features(audio)
            raw_embedding, averaged_embeddings = get_embedding(audio)
            genresJSON = get_genre(raw_embedding)
            with get_recommender_session() as rec_session:
                features = TrackFeatures(
                    dotnet_track_id=t.Id,
                    bpm=float(bpm),
                    energy=float(energy),
                    danceability=float(danceability),
                    key=key,
                    scale=scale,
                    genres=genresJSON,
                    embedding=averaged_embeddings.tolist(),
                )
                rec_session.add(features)
                rec_session.commit()
