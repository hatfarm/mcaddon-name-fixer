# Use the official Python image with Python 3.12
FROM python:3.12-slim

# Set the working directory inside the container
WORKDIR /app

# Create a folder named "uploads" (you can choose any name you like)
RUN mkdir uploads

WORKDIR /app/uploads

RUN mkdir cleaned

WORKDIR /app

# Copy the requirements.txt file into the container
COPY ./flask-web-server/requirements.txt .

# Install the required Python packages
RUN pip install -r requirements.txt

# Copy the entire app project into the container
COPY ./flask-web-server/ .

# Copy the MinecraftAddonNameFixer folder
COPY ./MinecraftAddonNameFixer /app/MinecraftAddonNameFixer

RUN apt-get update && apt-get install -y wget

RUN wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
RUN dpkg -i packages-microsoft-prod.deb
RUN rm packages-microsoft-prod.deb

# Install .NET SDK (you can specify a specific version if needed)
RUN apt-get update && apt-get install -y dotnet-sdk-8.0

# Build the MinecraftAddonNameFixer.Cli project
WORKDIR /app/MinecraftAddonNameFixer
RUN dotnet build --configuration Release

# This was used to figure out exactly where the build output goes :D
# RUN find "$PWD" -type f > ../uploads/cleaned/stuff.txt

RUN chmod +x /app/MinecraftAddonNameFixer/MinecraftAddonNameFixer.Cli/obj/Release/net8.0/MinecraftAddonNameFixer.Cli.dll


# Set environment variables
ENV NAME_FIXER_CLI_PATH=/app/MinecraftAddonNameFixer/MinecraftAddonNameFixer.Cli/obj/Release/net8.0/MinecraftAddonNameFixer.Cli.dll
ENV UPLOAD_FOLDER=/app/uploads

# Set the command to run your Flask app (adjust as needed)
CMD ["python", "/app/app.py"]
