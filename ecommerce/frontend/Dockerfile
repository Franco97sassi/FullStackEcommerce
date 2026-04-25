FROM node:20-alpine

WORKDIR /app


RUN apk add --no-cache curl
COPY package*.json ./
RUN npm install

COPY . .

EXPOSE 3000
HEALTHCHECK --interval=30s --timeout=5s --start-period=20s --retries=3 \
  CMD curl --fail http://localhost:3000 || exit 1

CMD ["npm", "run", "dev"]
CMD ["npm", "run", "dev"]