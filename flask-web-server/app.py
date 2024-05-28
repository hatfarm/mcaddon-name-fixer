from flask import Flask, request, send_file, render_template, redirect, url_for, jsonify
from werkzeug.utils import secure_filename
import os
import tempfile
import subprocess
from pathlib import Path
import multiprocessing
import time

app = Flask(__name__)
# Get the value of an environment variable
upload_folder = os.environ.get('UPLOAD_FOLDER')
name_fixer_executable = os.environ.get('NAME_FIXER_CLI_PATH')

# Set a default value if the key doesn't exist
if os.environ.get('UPLOAD_FOLDER') is None:
    upload_folder = tempfile.mkdtemp()

cleaned_folder = os.path.join(upload_folder, "cleaned")
Path(cleaned_folder).mkdir(parents=True, exist_ok=True)



def delete_old_files():
    # Calculate the timestamp for 8 hours ago
    eight_hours_ago = time.time() - 8 * 3600  # 8 hours in seconds

    # Remove files older than 8 hours
    for filename in os.listdir(cleaned_folder):
        filepath = os.path.join(cleaned_folder, filename)
        if os.path.isfile(filepath):
            file_mtime = os.path.getmtime(filepath)
            if file_mtime < eight_hours_ago:
                os.remove(filepath)


def file_cleaner():
    while True:
        delete_old_files()
        time.sleep(3600)


@app.route('/upload', methods=['POST'])
def upload_file():
    if 'file' not in request.files:
        return 'No file part'
    file = request.files['file']
    if file.filename == '':
        return 'No selected file'
    if file:
        filename = secure_filename(file.filename)
        file.save(os.path.join(upload_folder, filename))
        cleaned_filename = "cleaned_" + filename
        cleaned_filepath = os.path.join(cleaned_folder, cleaned_filename)
        source_filepath = os.path.join(upload_folder, filename)
        print("Executable, location:" + name_fixer_executable)
        result = subprocess.run(
            [
                "dotnet",
                name_fixer_executable,
                source_filepath,
                cleaned_filepath
            ],
            capture_output=True,
            text=True
        )

        time.sleep(1)
        return redirect(url_for('index'))


@app.route('/download/<filename>', methods=['GET'])
def download_file(filename):
    return send_file(os.path.join(cleaned_folder, filename), as_attachment=True)


@app.route('/')
def index():
    # Remove anything from the list that is not a file (directories, symlinks)
    files = os.listdir(cleaned_folder)
    files.sort(key=lambda x: os.path.getmtime(os.path.join(cleaned_folder, x)), reverse=True)
    return render_template('index.html', files=files)


@app.route('/get_cleaned_files', methods=['GET'])
def get_cleaned_files():
    files = os.listdir(cleaned_folder)
    files.sort(key=lambda x: os.path.getmtime(os.path.join(cleaned_folder, x)), reverse=True)
    return jsonify(files=files)


if __name__ == '__main__':
    scheduler_process = multiprocessing.Process(target=file_cleaner)
    scheduler_process.start()

    app.run(host='0.0.0.0')