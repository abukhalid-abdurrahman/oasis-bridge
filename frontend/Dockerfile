# Используем официальный образ Node.js
FROM node:22-alpine AS runner

# Устанавливаем рабочую директорию
WORKDIR /app

# Копируем файлы проекта
COPY . .

# Устанавливаем зависимости
RUN npm install --frozen-lockfile

# Открываем порт
EXPOSE 3000

# Запускаем билд и сервер при старте
CMD ["sh", "-c", "npm run build && npm run start"]
