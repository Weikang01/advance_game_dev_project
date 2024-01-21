import sqlite3


def create_database():
    # Connect to SQLite - This will create the database file if it doesn't exist
    conn = sqlite3.connect('user_data.db')

    # Create a cursor object using the cursor() method
    cursor = conn.cursor()

    # Create users table
    cursor.execute('''CREATE TABLE IF NOT EXISTS users (
                        id INTEGER PRIMARY KEY,
                        username TEXT NOT NULL UNIQUE,
                        password TEXT NOT NULL,
                        email TEXT NOT NULL,
                        phone TEXT
                      )''')

    # Create player_profiles table
    cursor.execute('''CREATE TABLE IF NOT EXISTS player_profiles (
                        id INTEGER PRIMARY KEY,
                        user_id INTEGER NOT NULL,
                        display_name TEXT NOT NULL,
                        avatar_url TEXT,
                        level INTEGER NOT NULL DEFAULT 1,
                        experience INTEGER NOT NULL DEFAULT 0,
                        FOREIGN KEY (user_id) REFERENCES users (id)
                      )''')

    # Commit your changes in the database
    conn.commit()

    # Close the connection
    conn.close()


if __name__ == "__main__":
    create_database()
