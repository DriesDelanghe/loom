FROM flyway/flyway:latest

WORKDIR /flyway

COPY sql /flyway/sql
COPY flyway.conf /flyway/conf/flyway.conf

ENTRYPOINT ["flyway", "migrate"]

