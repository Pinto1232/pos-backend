const { Sequelize } = require('sequelize');
const path = require('path');
const fs = require('fs');

// Load configuration
const env = process.env.NODE_ENV || 'development';
const config = require('./config/database.js')[env];

// Create Sequelize instance
const sequelize = new Sequelize(
  config.database,
  config.username,
  config.password,
  {
    host: config.host,
    dialect: config.dialect,
    logging: console.log
  }
);

// Test the connection
async function testConnection() {
  try {
    await sequelize.authenticate();
    console.log('Connection has been established successfully.');
    
    // Check if the Users table exists
    try {
      await sequelize.query('SELECT * FROM "Users" LIMIT 1');
      console.log('Users table exists.');
    } catch (error) {
      console.log('Users table does not exist. Creating it...');
      
      // Read the migration file
      const migrationPath = path.join(__dirname, 'src', 'migrations', '20250516043853-test-migration.js');
      const migration = require(migrationPath);
      
      // Run the migration
      await migration.up(sequelize.getQueryInterface(), Sequelize);
      console.log('Migration executed successfully.');
      
      // Read the seeder file
      const seederPath = path.join(__dirname, 'src', 'seeders', '20250516000000-demo-users.js');
      const seeder = require(seederPath);
      
      // Run the seeder
      await seeder.up(sequelize.getQueryInterface(), Sequelize);
      console.log('Seeder executed successfully.');
    }
    
    // Query the Users table
    const users = await sequelize.query('SELECT * FROM "Users"', { type: Sequelize.QueryTypes.SELECT });
    console.log('Users in the database:', users);
    
  } catch (error) {
    console.error('Unable to connect to the database:', error);
  } finally {
    await sequelize.close();
  }
}

testConnection();
