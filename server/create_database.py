import sqlite3


def create_database():
    # Connect to SQLite - This will create the database file if it doesn't exist
    conn = sqlite3.connect('user_data.db')

    # Create a cursor object using the cursor() method
    cursor = conn.cursor()

    # Create table as per requirement
    sql = '''CREATE TABLE users (
                id INTEGER PRIMARY KEY,
                username TEXT NOT NULL UNIQUE,
                password TEXT NOT NULL,
                email TEXT NOT NULL,
                phone TEXT
             )'''
    cursor.execute(sql)

    # Commit your changes in the database
    conn.commit()

    # Close the connection
    conn.close()


if __name__ == "__main__":
    create_database()
