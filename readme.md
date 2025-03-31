# Task Management Service

## Описание проекта
Task Management Service - это API-сервис, предназначенный для простого управления задачами пользователя.

## Функциональные возможности
- Создание, удаление и получение задач пользователя
- Каждая задача содержит название, описание, статус, дату создания и дату изменения
- Асинхронное взаимодействие между сервисами через очередь сообщений RabbitMQ
- Публикация событий при создании, изменении и удалении задач

## Требования и зависимости
- .NET 8 / ASP.NET Core
- PostgreSQL
- RabbitMQ
- Docker и Docker Compose

## Установка и запуск

### Запуск с использованием Docker Compose
1. Убедитесь, что у вас установлены Docker и Docker Compose
2. Клонируйте репозиторий
3. В корневой директории проекта выполните команду:
   ```
   docker-compose up
   ```
4. Swagger
API документация доступна через Swagger UI по адресу: http://localhost:8080/swagger/index.html

### Локальный запуск
1. Убедитесь, что у вас установлены .NET 8 SDK, PostgreSQL и RabbitMQ
2. Клонируйте репозиторий
3. Настройте строку подключения к базе данных в переменных окружения
4. Выполните миграции базы данных:
   ```
   dotnet ef database update --project TaskManagement.Infrastructure
   ```
5. Запустите проект API:
   ```
   dotnet run --project TaskManagement.Api
   ```
6. Запустите проект обработчика сообщений:
   ```
   dotnet run --project TaskManagement.MessageConsumer
   ```

## Структура проекта
- **TaskManagement.Api** - API-контроллеры и конфигурация
- **TaskManagement.Application** - сервисы приложения, DTO и интерфейсы
- **TaskManagement.Domain** - доменные модели, перечисления и события
- **TaskManagement.Infrastructure** - реализация репозиториев, контекст БД и миграции
- **TaskManagement.MessageConsumer** - обработчики событий из очереди сообщений
- **tests** - тесты проекта

## API Endpoints

### GET /api/tasks
Получение списка всех задач.

Пример ответа:
```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "title": "Task 1",
    "description": "Description for task 1",
    "status": "New",
    "createdAt": "2023-06-01T10:00:00Z",
    "updatedAt": "2023-06-01T10:00:00Z"
  }
]
```

### GET /api/tasks/{id}
Получение задачи по идентификатору.

Пример ответа:
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "title": "Task 1",
  "description": "Description for task 1",
  "status": "New",
  "createdAt": "2023-06-01T10:00:00Z",
  "updatedAt": "2023-06-01T10:00:00Z"
}
```

### POST /api/tasks
Создание новой задачи.

Пример запроса:
```json
{
  "title": "New Task",
  "description": "Description for new task"
}
```

### PUT /api/tasks/{id}
Обновление существующей задачи.

Пример запроса:
```json
{
  "title": "Updated Task",
  "description": "Updated description",
  "status": "InProgress"
}
```

### DELETE /api/tasks/{id}
Удаление задачи.

## События
При создании, обновлении и удалении задач генерируются соответствующие события:
- TaskCreatedEvent
- TaskUpdatedEvent
- TaskDeletedEvent

Эти события публикуются в RabbitMQ и обрабатываются соответствующими обработчиками.

## Логирование и наблюдаемость
Проект использует OpenTelemetry для трассировки и логирования.