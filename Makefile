# forth16 Makefile

all: vm

run: vm
	@./vm

CC = clang
CFLAGS = -Wall -Werror
CFLAGS += -Ofast

SOURCES = src/main.c src/cpu.c src/lib.c src/disk.c
HEADERS = src/vm.h
LIBS = -ledit -ldl

vm: $(SOURCES) $(HEADERS)
	$(CC) $(CFLAGS) $(SOURCES) $(LIBS) -o $@

clean:
	@rm -f vm
