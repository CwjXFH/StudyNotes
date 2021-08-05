FROM redis:latest

ARG cell_dir=/lib/redis_modules/redis_cell
RUN mkdir -p ${cell_dir}
WORKDIR ${cell_dir}

RUN apt-get update \
     && apt-get -y install wget \
     && wget https://github.com/brandur/redis-cell/releases/download/v0.3.0/redis-cell-v0.3.0-x86_64-unknown-linux-gnu.tar.gz \
     && tar -zxvf redis-cell-v0.3.0-x86_64-unknown-linux-gnu.tar.gz \
     && rm redis-cell-v0.3.0-x86_64-unknown-linux-gnu.tar.gz \
     && apt-get -y remove wget \
     && apt -y autoremove \
     && ls -alh

ENTRYPOINT ["redis-server","--loadmodule","/lib/redis_modules/redis_cell/libredis_cell.so"]

