const { Sequelize } = require('sequelize');

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
    logging: false
  }
);

async function checkTable() {
  try {
    // Connect to the database
    await sequelize.authenticate();
    console.log('Connection has been established successfully.');
    
    // Query to get table structure
    const [results] = await sequelize.query(`
      SELECT column_name, data_type, column_default, is_nullable
      FROM information_schema.columns
      WHERE table_name = 'Users'
      ORDER BY ordinal_position;
    `);
    
    console.log('Users table structure:');
    console.table(results);
    
    // Query to get some sample data
    const [users] = await sequelize.query('SELECT * FROM "Users" LIMIT 5;');
    console.log('\nSample data from Users table:');
    console.table(users);
    
  } catch (error) {
    console.error('Error:', error.message);
  } finally {
    await sequelize.close();
  }
}

checkTable();
