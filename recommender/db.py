from sqlalchemy import create_engine, event, text
from sqlalchemy.orm import sessionmaker

from config import DATABASE_URL_RECOMMENDER, DATABASE_URL_READONLY

recommender_engine = create_engine(DATABASE_URL_RECOMMENDER)
readonly_engine = create_engine(DATABASE_URL_READONLY)

recommender_session_maker = sessionmaker(recommender_engine)
readonly_session_maker = sessionmaker(readonly_engine)
def get_recommender_session():
    return recommender_session_maker()
def get_readonly_session():
    return readonly_session_maker()

@event.listens_for(readonly_engine, "begin")
def set_readonly(conn):
    conn.execute(text("SET default_transaction_read_only = ON"))
