const fs = require('fs');
const path = require('path');
const { execSync } = require('child_process');

// Define the required directories
const directories = [
  'src',
  'src/models',
  'src/migrations',
  'src/seeders',
  'config'
];

// Create directories if they don't exist
directories.forEach(dir => {
  const dirPath = path.join(__dirname, dir);
  if (!fs.existsSync(dirPath)) {
    console.log(`Creating directory: ${dir}`);
    fs.mkdirSync(dirPath, { recursive: true });
  } else {
    console.log(`Directory already exists: ${dir}`);
  }
});

// Check if database.js exists in config directory
const databaseConfigPath = path.join(__dirname, 'config', 'database.js');
if (!fs.existsSync(databaseConfigPath)) {
  console.log('Creating config/database.js');
  const databaseConfig = `
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
`;
  fs.writeFileSync(databaseConfigPath, databaseConfig);
}

// Check if .sequelizerc exists
const sequelizeRcPath = path.join(__dirname, '.sequelizerc');
if (!fs.existsSync(sequelizeRcPath)) {
  console.log('Creating .sequelizerc');
  const sequelizeRc = `
const path = require('path');

module.exports = {
  'config': path.resolve('config', 'database.js'),
  'models-path': path.resolve('src', 'models'),
  'seeders-path': path.resolve('src', 'seeders'),
  'migrations-path': path.resolve('src', 'migrations')
};
`;
  fs.writeFileSync(sequelizeRcPath, sequelizeRc);
}

// Check if models/index.js exists
const modelsIndexPath = path.join(__dirname, 'src', 'models', 'index.js');
if (!fs.existsSync(modelsIndexPath)) {
  console.log('Creating src/models/index.js');
  const modelsIndex = `
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
`;
  fs.writeFileSync(modelsIndexPath, modelsIndex);
}

// Check if user.js exists
const userModelPath = path.join(__dirname, 'src', 'models', 'user.js');
if (!fs.existsSync(userModelPath)) {
  console.log('Creating src/models/user.js');
  const userModel = `
'use strict';

module.exports = (sequelize, DataTypes) => {
  const User = sequelize.define('User', {
    username: {
      type: DataTypes.STRING,
      allowNull: false,
      unique: true
    },
    email: {
      type: DataTypes.STRING,
      allowNull: false,
      unique: true,
      validate: {
        isEmail: true
      }
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

  User.associate = function(models) {
    // Define associations here if needed
  };

  return User;
};
`;
  fs.writeFileSync(userModelPath, userModel);
}

// Check if migration file exists
const migrationPath = path.join(__dirname, 'src', 'migrations', '20250516043853-test-migration.js');
if (!fs.existsSync(migrationPath)) {
  console.log('Creating src/migrations/20250516043853-test-migration.js');
  const migration = `
'use strict';

/** @type {import('sequelize-cli').Migration} */
module.exports = {
  async up (queryInterface, Sequelize) {
    await queryInterface.createTable('Users', {
      id: {
        allowNull: false,
        autoIncrement: true,
        primaryKey: true,
        type: Sequelize.INTEGER
      },
      username: {
        type: Sequelize.STRING,
        allowNull: false,
        unique: true
      },
      email: {
        type: Sequelize.STRING,
        allowNull: false,
        unique: true
      },
      password: {
        type: Sequelize.STRING,
        allowNull: false
      },
      isActive: {
        type: Sequelize.BOOLEAN,
        defaultValue: true
      },
      createdAt: {
        allowNull: false,
        type: Sequelize.DATE
      },
      updatedAt: {
        allowNull: false,
        type: Sequelize.DATE
      }
    });
  },

  async down (queryInterface, Sequelize) {
    await queryInterface.dropTable('Users');
  }
};
`;
  fs.writeFileSync(migrationPath, migration);
}

// Check if seeder file exists
const seederPath = path.join(__dirname, 'src', 'seeders', '20250516000000-demo-users.js');
if (!fs.existsSync(seederPath)) {
  console.log('Creating src/seeders/20250516000000-demo-users.js');
  const seeder = `
'use strict';

/** @type {import('sequelize-cli').Migration} */
module.exports = {
  async up (queryInterface, Sequelize) {
    await queryInterface.bulkInsert('Users', [
      {
        username: 'admin',
        email: 'admin@example.com',
        password: 'password123', // In a real app, this should be hashed
        isActive: true,
        createdAt: new Date(),
        updatedAt: new Date()
      },
      {
        username: 'user1',
        email: 'user1@example.com',
        password: 'password123', // In a real app, this should be hashed
        isActive: true,
        createdAt: new Date(),
        updatedAt: new Date()
      },
      {
        username: 'user2',
        email: 'user2@example.com',
        password: 'password123', // In a real app, this should be hashed
        isActive: false,
        createdAt: new Date(),
        updatedAt: new Date()
      }
    ], {});
  },

  async down (queryInterface, Sequelize) {
    await queryInterface.bulkDelete('Users', null, {});
  }
};
`;
  fs.writeFileSync(seederPath, seeder);
}

console.log('Sequelize setup fixed successfully!');
console.log('You can now run the sequelize-test.js script to test the connection:');
console.log('node sequelize-test.js');
