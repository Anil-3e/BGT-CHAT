-- BGT Chat SQLite database schema
-- The ASP.NET Core server creates this database automatically.

-- Stores room codes that users can join.
create table if not exists rooms (
    room_code text primary key collate nocase,
    room_name text not null,
    created_at text not null default current_timestamp
);

-- Stores optional user display names.
create table if not exists users_profile (
    id integer primary key autoincrement,
    username text not null unique collate nocase,
    created_at text not null default current_timestamp
);

-- Stores every chat message and connects it to a room.
create table if not exists messages (
    id integer primary key autoincrement,
    room_code text not null collate nocase,
    username text not null check (length(trim(username)) > 0),
    message_text text not null check (length(trim(message_text)) > 0),
    created_at text not null default current_timestamp,
    foreign key (room_code) references rooms (room_code)
        on update cascade
        on delete cascade
);

create index if not exists messages_room_created_at_idx
    on messages (room_code, created_at);

insert or ignore into rooms (room_code, room_name) values
    ('BGT2026', 'BGT General Chat'),
    ('WEB101', 'Web Development'),
    ('CSHARP1', 'C# Programming'),
    ('DATABASE', 'Database Class');
