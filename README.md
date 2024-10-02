# WebAppRabbitMQ

# Iniciar uma instância do RMQ com as configuraões iniciais no modo local
docker run -d --hostname rmq --name rabbit-server -p 8080:15672 -p 5672:5672 rabbitmq:3-management

# Instância disponível:
http://localhost:8080/
guest
guest
