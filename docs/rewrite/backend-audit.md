# Backend Audit (Current Node/Express + Socket.IO)

This summarizes the current backend operations, events, and dependencies as input to the rewrite.

## Host-level operations (root namespace command)
- create
- create_unconventional_server
- download (profile asset)
- build_jar (BuildTools)
- delete_build
- copy_to_server
- refresh_server_list
- refresh_profile_list
- create_from_archive

## Server-level operations (per-server command)
- start
- stop
- restart
- stop_and_backup
- kill
- stuff (send console input)
- saveall
- saveall_latest_log
- archive
- archive_with_commit
- backup
- restore
- prune
- list_archives
- list_increments
- list_increment_sizes
- modify_sp (server.properties)
- modify_sc (server.config)
- property (get single property)
- chown
- accept_eula
- copy_profile
- run_installer (FTBInstall.sh)
- renice
- delete (server files, archives, backups)
- ping (server ping)
- query (server query)

## Scheduled jobs (cron)
- create
- delete
- start
- suspend
- actions: backup, archive, archive_with_commit, stuff

## File/process dependencies
- screen (detached server process control)
- rdiff-backup (incremental backups, restore, prune)
- rsync (profile copies, sync)
- procfs (/proc for process discovery and metrics)
- tar/zip/unzip (imports and profiles)

## Host events (emitted to root namespace)
- whoami
- commit_msg
- change_locale
- optional_columns
- track_server
- untrack_server
- host_diskspace
- host_heartbeat
- profile_list
- spigot_list
- file_progress
- archive_list (importables)
- locale_list
- user_list
- group_list
- build_jar_output
- host_notice

## Server events (emitted to /{server_name} namespace)
- heartbeat
- page_data
- archives
- increments
- increment_sizes
- tail_data
- file head
- notices
- server_ack
- server_fin
- server-icon.png
- server.properties
- server.config
- config.yml
- cron.config
- eula

## Client socket events (incoming)
Root namespace:
- command

Server namespace:
- command
- get_file_contents
- get_available_tails
- property
- page_data
- archives
- increments
- increment_sizes
- cron
- server.properties
- server.config
- cron.config
- server-icon.png
- config.yml
- req_server_activity

## Legacy HTTP endpoints
- GET /
- GET /admin/index.html
- GET /login
- POST /auth
- GET /logout
- POST /admin/command
- ALL /api/:server_name/:command
