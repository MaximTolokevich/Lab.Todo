FROM mcr.microsoft.com/mssql/server:2017-latest

USER root

RUN apt-get update \
    && apt-get install unzip -y

RUN wget -progress=bar:force -q -O sqlpackage.zip https://go.microsoft.com/fwlink/?linkid=2157202 \
    && unzip -qq sqlpackage.zip -d /opt/sqlpackage \
    && chmod +x /opt/sqlpackage/sqlpackage

COPY Lab.Todo.Database/bin/Debug/Lab.Todo.Database.dacpac /tmp/Lab.Todo.Database/Lab.Todo.Database.dacpac

ARG DBNAME=ToDoDatabase
ARG PASSWORD=tmpPassword123

ENV ACCEPT_EULA=Y
ENV SA_PASSWORD=$PASSWORD

RUN ( /opt/mssql/bin/sqlservr & ) | grep -q "Service Broker manager has started" \
    && /opt/sqlpackage/sqlpackage /a:Publish /tsn:. /tdn:$DBNAME /tu:sa /tp:$SA_PASSWORD /sf:/tmp/Lab.Todo.Database/Lab.Todo.Database.dacpac \
    && rm /tmp/Lab.Todo.Database/Lab.Todo.Database.dacpac \
    && pkill sqlservr