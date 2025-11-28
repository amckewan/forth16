# forth16 Makefile

all: vm

run: vm
	@./vm

CC = clang
CFLAGS = -Wall -Werror
CFLAGS += -Ofast

SOURCES = src/main.c src/cpu.c src/lib.c src/console.c src/disk.c src/bios.c 
HEADERS = src/vm.h
LIBS = -ledit -ldl

vm: $(SOURCES) $(HEADERS)
	$(CC) $(CFLAGS) $(SOURCES) $(LIBS) -o $@

blk2txt: tools/blk2txt.c
	$(CC) $(CFLAGS) tools/blk2txt.c -o $@

blocks.fb: blk2txt blocks.fb.fx
	./blk2txt d < blocks.fb.fx > blocks.fb

encode: blk2txt
	./blk2txt e < blocks.fb > blocks.fb.fx

clean:
	@rm -f vm blk2txt a.out
