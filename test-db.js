const { Pool } = require('pg');

// Load configuration
const env = process.env.NODE_ENV || 'development';
const config = require('./config/database.js')[env];

// Create a connection pool
const pool = new Pool({
  user: config.username,
  host: config.host,
  database: config.database,
  password: config.password,
  port: 5432,
});

// Test the connection
async function testConnection() {
  try {
    const client = await pool.connect();
    console.log('Connected to the database successfully!');
    
    // Check if the Users table exists
    try {
      const result = await client.query(`
        SELECT EXISTS (
          SELECT FROM information_schema.tables 
          WHERE table_schema = 'public'
          AND table_name = 'Users'
        );
      `);
      
      if (result.rows[0].exists) {
        console.log('Users table exists.');
        
        // Query the Users table
        const users = await client.query('SELECT * FROM "Users"');
        console.log('Users in the database:', users.rows);
      } else {
        console.log('Users table does not exist.');
        
        // Create the Users table
        await client.query(`
          CREATE TABLE IF NOT EXISTS "Users" (
            id SERIAL PRIMARY KEY,
            username VARCHAR(255) NOT NULL UNIQUE,
            email VARCHAR(255) NOT NULL UNIQUE,
            password VARCHAR(255) NOT NULL,
            "isActive" BOOLEAN DEFAULT true,
            "createdAt" TIMESTAMP NOT NULL,
            "updatedAt" TIMESTAMP NOT NULL
          );
        `);
        console.log('Users table created successfully.');
        
        // Insert sample users
        await client.query(`
          INSERT INTO "Users" (username, email, password, "isActive", "createdAt", "updatedAt")
          VALUES 
            ('admin', 'admin@example.com', 'password123', true, NOW(), NOW()),
            ('user1', 'user1@example.com', 'password123', true, NOW(), NOW()),
            ('user2', 'user2@example.com', 'password123', false, NOW(), NOW());
        `);
        console.log('Sample users inserted successfully.');
      }
    } catch (error) {
      console.error('Error checking/creating Users table:', error);
    }
    
    client.release();
  } catch (error) {
    console.error('Unable to connect to the database:', error);
  } finally {
    await pool.end();
  }
}

testConnection();
