const { Sequelize } = require('sequelize');
const path = require('path');
const fs = require('fs');
const Umzug = require('umzug');

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

// Configure Umzug for migrations
const umzug = new Umzug({
  migrations: {
    path: path.join(__dirname, 'src', 'migrations'),
    pattern: /\.js$/,
    params: [
      sequelize.getQueryInterface(),
      Sequelize
    ]
  },
  storage: 'sequelize',
  storageOptions: {
    sequelize: sequelize
  }
});

// Run migrations
async function runMigrations() {
  try {
    await sequelize.authenticate();
    console.log('Connection has been established successfully.');
    
    // Run pending migrations
    const migrations = await umzug.up();
    console.log('Migrations executed:', migrations.map(m => m.file));
    
    // Run seeders
    const seedersPath = path.join(__dirname, 'src', 'seeders');
    const seeders = fs.readdirSync(seedersPath)
      .filter(file => file.endsWith('.js'))
      .map(file => path.join(seedersPath, file));
    
    for (const seederPath of seeders) {
      const seeder = require(seederPath);
      await seeder.up(sequelize.getQueryInterface(), Sequelize);
      console.log('Seeder executed:', path.basename(seederPath));
    }
    
    // Query the Users table
    const users = await sequelize.query('SELECT * FROM "Users"', { type: Sequelize.QueryTypes.SELECT });
    console.log('Users in the database:', users);
    
  } catch (error) {
    console.error('Error running migrations:', error);
  } finally {
    await sequelize.close();
  }
}

runMigrations();
