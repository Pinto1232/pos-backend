const { Sequelize, DataTypes } = require('sequelize');

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

// Define the User model directly
const User = sequelize.define('User', {
  username: {
    type: DataTypes.STRING,
    allowNull: false,
    unique: true
  },
  email: {
    type: DataTypes.STRING,
    allowNull: false,
    unique: true
  },
  password: {
    type: DataTypes.STRING,
    allowNull: false
  },
  isActive: {
    type: DataTypes.BOOLEAN,
    defaultValue: true
  }
}, {
  tableName: 'Users'
});

// Test the connection and model
async function testSequelize() {
  try {
    // Test the connection
    await sequelize.authenticate();
    console.log('Connection has been established successfully.');
    
    // Sync the model with the database (create the table if it doesn't exist)
    await User.sync({ alter: true });
    console.log('User model synchronized with database.');
    
    // Create a user if none exists
    const count = await User.count();
    if (count === 0) {
      await User.bulkCreate([
        {
          username: 'admin',
          email: 'admin@example.com',
          password: 'password123',
          isActive: true
        },
        {
          username: 'user1',
          email: 'user1@example.com',
          password: 'password123',
          isActive: true
        }
      ]);
      console.log('Sample users created.');
    } else {
      console.log(`${count} users already exist in the database.`);
    }
    
    // Query all users
    const users = await User.findAll();
    console.log('Users in the database:');
    users.forEach(user => {
      console.log(`- ${user.username} (${user.email}), Active: ${user.isActive}`);
    });
    
  } catch (error) {
    console.error('Error testing Sequelize:', error);
  } finally {
    await sequelize.close();
  }
}

// Run the test
testSequelize();
