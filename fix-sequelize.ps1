# PowerShell script to fix Sequelize setup

Write-Host "Fixing Sequelize setup..." -ForegroundColor Cyan

# Check if src directory exists, create if not
if (-not (Test-Path "src")) {
    Write-Host "Creating src directory..." -ForegroundColor Yellow
    New-Item -ItemType Directory -Path "src" -Force | Out-Null
}

# Check if src/models directory exists, create if not
if (-not (Test-Path "src\models")) {
    Write-Host "Creating src\models directory..." -ForegroundColor Yellow
    New-Item -ItemType Directory -Path "src\models" -Force | Out-Null
}

# Check if src/migrations directory exists, create if not
if (-not (Test-Path "src\migrations")) {
    Write-Host "Creating src\migrations directory..." -ForegroundColor Yellow
    New-Item -ItemType Directory -Path "src\migrations" -Force | Out-Null
}

# Check if src/seeders directory exists, create if not
if (-not (Test-Path "src\seeders")) {
    Write-Host "Creating src\seeders directory..." -ForegroundColor Yellow
    New-Item -ItemType Directory -Path "src\seeders" -Force | Out-Null
}

# Check if config directory exists, create if not
if (-not (Test-Path "config")) {
    Write-Host "Creating config directory..." -ForegroundColor Yellow
    New-Item -ItemType Directory -Path "config" -Force | Out-Null
}

# Check if database.js exists in config directory
if (-not (Test-Path "config\database.js")) {
    Write-Host "Creating config\database.js..." -ForegroundColor Yellow
    @"
module.exports = {
  "development": {
    "username": "pos_user",
    "password": "rj200100p",
    "database": "pos_system",
    "host": "localhost",
    "dialect": "postgres"
  },
  "test": {
    "username": "pos_user",
    "password": "rj200100p",
    "database": "pos_system_test",
    "host": "localhost",
    "dialect": "postgres"
  },
  "production": {
    "username": process.env.DB_USERNAME || "pos_user",
    "password": process.env.DB_PASSWORD || "rj200100p",
    "database": process.env.DB_NAME || "pos_system",
    "host": process.env.DB_HOST || "localhost",
    "dialect": "postgres"
  }
}
"@ | Out-File -FilePath "config\database.js" -Encoding utf8
}

# Check if .sequelizerc exists
if (-not (Test-Path ".sequelizerc")) {
    Write-Host "Creating .sequelizerc..." -ForegroundColor Yellow
    @"
const path = require('path');

module.exports = {
  'config': path.resolve('config', 'database.js'),
  'models-path': path.resolve('src', 'models'),
  'seeders-path': path.resolve('src', 'seeders'),
  'migrations-path': path.resolve('src', 'migrations')
};
"@ | Out-File -FilePath ".sequelizerc" -Encoding utf8
}

# Check if models/index.js exists
if (-not (Test-Path "src\models\index.js")) {
    Write-Host "Creating src\models\index.js..." -ForegroundColor Yellow
    @"
'use strict';

const fs = require('fs');
const path = require('path');
const Sequelize = require('sequelize');
const process = require('process');
const basename = path.basename(__filename);
const env = process.env.NODE_ENV || 'development';
const config = require('../../config/database.js')[env];
const db = {};

let sequelize;
if (config.use_env_variable) {
  sequelize = new Sequelize(process.env[config.use_env_variable], config);
} else {
  sequelize = new Sequelize(config.database, config.username, config.password, config);
}

fs
  .readdirSync(__dirname)
  .filter(file => {
    return (
      file.indexOf('.') !== 0 &&
      file !== basename &&
      file.slice(-3) === '.js' &&
      file.indexOf('.test.js') === -1
    );
  })
  .forEach(file => {
    const model = require(path.join(__dirname, file))(sequelize, Sequelize.DataTypes);
    db[model.name] = model;
  });

Object.keys(db).forEach(modelName => {
  if (db[modelName].associate) {
    db[modelName].associate(db);
  }
});

db.sequelize = sequelize;
db.Sequelize = Sequelize;

module.exports = db;
"@ | Out-File -FilePath "src\models\index.js" -Encoding utf8
}

Write-Host "Sequelize setup fixed successfully!" -ForegroundColor Green
Write-Host "You can now run Sequelize CLI commands." -ForegroundColor Green
