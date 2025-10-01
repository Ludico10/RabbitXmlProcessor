# RabbitXmlProcessor

[**Описание на русском языке можно найти здесь**](https://github.com/Ludico10/RabbitXmlProcessor/edit/master/README.ru.md)

The system consists of two services:

* **FileParserService** – monitors the 'input' folder, parses XML files, and sends data to RabbitMQ.
* **DataProcessorService** – receives messages from RabbitMQ and stores them in an SQLite database.
* **Shared** – a shared class library for services, containing definitions of structures found in XML files and methods for service interaction via RabbitMQ.

---

## Launch Algorithm
! You will need Docker to run the project.

1. Clone the repository:

```
git clone https://github.com/Ludico10/RabbitXmlProcessor.git
cd RabbitXmlProcessor
```

2. Build and start the services:

```
docker-compose up --build
```

3. It takes about 30 seconds for the full startup, after which you can see the logs:

```
fileparser-1 | info: DirectoryMonitor[0]
fileparser-1 | Parsing started. Directory: ./input, interval: 1000ms.
```
```
dataprocessor-1 | info: Shared.RabbitPostman[0]
dataprocessor-1 | Waiting for data started.
```

4. Access:

* RabbitMQ Management UI → [http://localhost:15672](http://localhost:15672)
login: `guest`, password: `guest`
* The SQLite database is saved in the `DataProcessorService/db` folder
* To start processing, place the files in the `FileParserService/input` directory

5. To stop services, use the *ctrl + C* shortcut.

---

## Configuration Settings

Each service uses the `appsettings.json` file. It contains:
* *RabbitMq* - RabbitMq settings. **Must remain the same in both files and in docker-compose**.
* *Logging* - Logging settings (logs can be output to the console or to a file).
* *DatabasePath* - Specifies the relative path to the database. Only present in the DataProcessorService file. **It is recommended to change only the database name while preserving the path**, as otherwise you will need to make additional changes to the Dockerfile.
* *InputDirectory* - The relative path to the FileParserService directory from which files are read. **Changing this will also require editing the Dockerfile**.
* *MonitoringInterval* - The time interval in milliseconds between directory scans. Present only in the FileParserService service file.
* *useFileHash* - A flag that provides more accurate file change tracking. It is recommended to set this to true only if you expect incoming files to be renamed, otherwise it may slow down the program. Present only in the FileParserService service file.

---

## Configuration Changes
Be careful not to change parameters that control paths without understanding the services' Dockerfiles.

1. Edit `appsettings.json`.
2. Rebuild the service:

```
docker-compose up --build -d
```

---

## Logs

You can view container logs:

```
docker logs -f rabbitxmlprocessor-fileparser-1
docker logs -f rabbitxmlprocessor-dataprocessor-1
```

They are also stored in files in containers:
* */logs/fileparser.log*
* */logs/dataprocessor.log*

---

## Testing

In the **tests** directory, you can find Unit tests for each library and service. We recommend reading them for a deeper understanding of the code.

## Thank you for your attention!
