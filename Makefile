# forth16 Makefile

all: vm

run: vm
	@./vm

CC = clang
CFLAGS = -Wall -Werror
CFLAGS += -Ofast

SOURCES = src/main.c src/cpu.c src/lib.c src/console.c src/disk.c src/swi.c 
HEADERS = src/vm.h
LIBS = -ledit -ldl

vm: $(SOURCES) $(HEADERS)
	$(CC) $(CFLAGS) $(SOURCES) $(LIBS) -o $@

blk2txt: tools/blk2txt.c
	$(CC) $(CFLAGS) tools/blk2txt.c -o $@

blocks.fb: blk2txt blocks.fb.fs
	./blk2txt d  < blocks.fb.fs  > blocks.fb

encode: blk2txt
	./blk2txt e  < blocks.fb  > blocks.fb.fs

clean:
	@rm -f vm blk2txt
