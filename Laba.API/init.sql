CREATE
DATABASE test_db OWNER postgres TEMPLATE template0 ENCODING 'UTF8';

CREATE TABLE users
(
    id              uuid PRIMARY KEY,
    username        varchar(25) UNIQUE NOT NULL,
    email           varchar(50) UNIQUE NOT NULL,
    hashed_password text               NOT NULL,
    created_at      TIMESTAMP          NOT NULL,
    is_deleted      boolean            NOT NULL DEFAULT FALSE
);
CREATE INDEX users_username_index ON users (username);
CREATE INDEX users_email_index ON users (email);

CREATE TABLE tokens
(
    user_id              uuid NOT NULL,
    hashed_refresh_token text NOT NULL,
    FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE CASCADE
);