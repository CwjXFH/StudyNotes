import asyncio
from fastapi import FastAPI
from hero import Hero
from hero_dbcontext import HeroDbContext
from contextlib import contextmanager
from typing import Union
import os

app = FastAPI()


@app.head("/healthcheck")
@app.get("/healthcheck")
def healthcheck() -> None:
    return {"msg": "ok"}


app_env = os.environ.get('APP_ENVIRONMENT')
if app_env == 'Production':
    db_path = "/home/db/pdemo.db"
else:
    cur_dir = os.getcwd()
    db_path = os.path.join(cur_dir, "../docs/pdemo.db")

# @app.get("/{id}", summary="获取英雄信息", tags=["三国"], include_in_schema=True)
# async def get(id: int):
#     async with HeroDbContext(db_path) as db:
#         db_r = db.get_by_id(id)
#         return {"successed": False if db_r is None else True, "data": db_r}

dbcontext = HeroDbContext(db_path)

@app.get("/{id}", summary="获取英雄信息（复用连接）", tags=["三国"], include_in_schema=True)
async def get(id: int):
    db_r = dbcontext.get_by_id(id)
    return {"successed": False if db_r is None else True, "data": db_r}


@app.get("/name/{name}", summary="根据名字获取英雄信息", tags=["三国"], include_in_schema=True)
async def get(name: str):
    async with HeroDbContext(db_path) as db:
        db_r = db.get_by_name(name)
        return {"successed": False if db_r is None else True, "data": db_r}
