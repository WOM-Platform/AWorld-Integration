SHELL := /bin/bash

DC := docker-compose -f docker-compose.yml -f docker-compose.custom.yml
DC_RUN := ${DC} run --rm

include config.env
export

.PHONY: confirmation
confirmation:
	@echo -n 'Are you sure? [y|N] ' && read ans && [ $$ans == y ]

.PHONY: cmd
cmd:
	@echo 'Docker-Compose command:'
	@echo '${DC}'

.PHONY: up
up:
	${DC} up -d web
	${DC} ps
	@echo
	@echo 'WOM AWorld integration is now up'
	@echo

.PHONY: ps
ps:
	${DC} ps

.PHONY: rs
rs:
	${DC} restart

.PHONY: rebuild
rebuild:
	${DC} rm -sf web
	${DC} build web
	${DC} up -d web

.PHONY: stop
stop:
	${DC} stop

.PHONY: rm
rm:
	${DC} rm -fs

.PHONY: logs
logs:
	docker logs -f $(shell ${DC} ps -q web)
