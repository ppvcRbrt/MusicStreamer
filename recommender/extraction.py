import essentia
from essentia.standard import MonoLoader, TensorflowPredictEffnetDiscogs, TensorflowPredict2D
import numpy as np
import json

with open('mtg_jamendo_genre-discogs-effnet-1.json') as f:
    metadata = json.load(f)
genre_labels = metadata['classes']
rhythm = essentia.standard.RhythmExtractor2013()
model = TensorflowPredictEffnetDiscogs(graphFilename='discogs-effnet-bs64-1.pb', output='PartitionedCall:1')
genre_model = TensorflowPredict2D(graphFilename='mtg_jamendo_genre-discogs-effnet-1.pb', input='model/Placeholder', output='model/Sigmoid')


def load_audio(filename):
    loader = MonoLoader(filename=filename, sampleRate=16000, resampleQuality=4)
    return loader()

def get_basic_features(audio):
    bpm, beats, confidence, estimates, intervals = rhythm(audio)
    energy = essentia.standard.Energy()(audio)
    danceability, _ = essentia.standard.Danceability()(audio)
    key, scale, strength = essentia.standard.KeyExtractor()(audio)
    return bpm, energy, danceability, key, scale

def get_embedding(audio):
    raw_embeddings = model(audio)
    averaged_embeddings = np.mean(raw_embeddings, axis=0)
    return raw_embeddings, averaged_embeddings


def get_genre(embeddings, threshold=0.15, max_genres=5):
    predictions = genre_model(embeddings)
    avg_predictions = np.mean(predictions, axis=0)
    top_indices = np.argsort(avg_predictions)[::-1][:max_genres]

    genres = []
    for idx in top_indices:
        score = float(avg_predictions[idx])
        if score < threshold:
            break
        genres.append({"genre": genre_labels[idx], "score": round(score, 4)})

    return genres
