'use strict';

/** @type {import('sequelize-cli').Migration} */
module.exports = {
  async up (queryInterface, Sequelize) {
    // Add a test column to the Users table
    await queryInterface.addColumn('Users', 'testColumn', {
      type: Sequelize.STRING,
      allowNull: true,
      defaultValue: 'Test Value'
    });
  },

  async down (queryInterface, Sequelize) {
    // Remove the test column
    await queryInterface.removeColumn('Users', 'testColumn');
  }
};
