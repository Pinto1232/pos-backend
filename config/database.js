
// Database configuration for the POS system
// Added this comment to test CI workflow
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

