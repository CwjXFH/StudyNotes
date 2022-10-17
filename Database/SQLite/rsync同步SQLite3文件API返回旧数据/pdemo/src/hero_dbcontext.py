import sqlite3
from sqlite3 import Cursor
from typing import Optional, Union
from hero import Hero


class HeroDbContext:
    def __init__(self, conn_str):
        self.__conn_str = conn_str
        self.__conn = sqlite3.connect(conn_str)

    async def __aenter__(self):
        if self.__conn is None:
            self.__conn = sqlite3.connect(self.__conn_str)
        return self

    async def __aexit__(self, ex_type, ex_value, ex_traceback) -> bool:
        if self.__conn is not None:
            self.__conn.close()
        return True

    def insert(self, entity: Hero):
        try:
            cur = self.__conn.cursor()
            cur.execute("INSERT INTO Heros(Name) VALUES(?)", [entity.name])
            self.__conn.commit()
        finally:
            cur.close()

    def get_by_name(self, name: str) -> Optional[Hero]:
        try:
            cur = self.__conn.cursor()
            r = cur.execute("SELECT * FROM Heros WHERE Name=?", [name])
            return self.__handle_db_result(r)
        finally:
            cur.close()

    def get_by_id(self, id: int) -> Union[Hero, None]:
        try:
            cur = self.__conn.cursor()
            r = cur.execute("SELECT * FROM Heros WHERE Id=:id", [id])
            return self.__handle_db_result(r)
        finally:
            cur.close()

    def __handle_db_result(self, result: Cursor) -> Optional[Hero]:
        if result is None:
            return None
        data = result.fetchone()
        return None if data is None else Hero(data[0], data[1])
