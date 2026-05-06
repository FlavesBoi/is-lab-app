# RUNBOOK — IS Lab App Stack

## Проверка статуса сервисов
```bash
cd /home/deployer/deploy/is-stack
sudo docker compose ps
```

## Просмотр логов
```bash
sudo docker compose logs --tail 50 app
sudo docker compose logs --tail 50 mssql
```

## Проверка доступности
```bash
curl http://127.0.0.1:5000/health
curl http://127.0.0.1:5000/version
curl http://127.0.0.1:5000/db/ping
```

## Обновление (смена тега)
```bash
# В .env обновить APP_VERSION, затем:
sudo docker compose pull
sudo docker compose up -d
```

## Откат
```bash
# В .env вернуть предыдущий тег, затем:
sudo docker compose pull
sudo docker compose up -d
```

## Резервное копирование
```bash
sudo docker compose exec mssql /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P "${SA_PASSWORD}" -No \
  -Q "BACKUP DATABASE IsLabDb TO DISK = N'/var/opt/mssql/backup/IsLabDb_full.bak' WITH INIT, COMPRESSION;"
# Файл хранится на хосте: /opt/backups/mssql/IsLabDb_full.bak
```

## Проверка восстановления
```bash
sudo docker compose exec mssql /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P "${SA_PASSWORD}" -No \
  -Q "RESTORE DATABASE IsLabDb_RestoreTest FROM DISK = N'/var/opt/mssql/backup/IsLabDb_full.bak' WITH MOVE 'IsLabDb' TO '/var/opt/mssql/data/IsLabDb_RestoreTest.mdf', MOVE 'IsLabDb_log' TO '/var/opt/mssql/data/IsLabDb_RestoreTest_log.ldf', REPLACE;"
sudo docker compose exec mssql /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P "${SA_PASSWORD}" -No \
  -Q "USE IsLabDb_RestoreTest; SELECT COUNT(*) FROM TestTable;"
sudo docker compose exec mssql /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P "${SA_PASSWORD}" -No \
  -Q "DROP DATABASE IsLabDb_RestoreTest;"
```
