// VM interface

#include <stdint.h>

// Rust int types
typedef   int8_t  i8;
typedef  uint8_t  u8;
typedef  int16_t  i16;
typedef uint16_t  u16;
typedef  int32_t  i32;
typedef uint32_t  u32;
typedef  int64_t  i64;
typedef uint64_t  u64;

extern u8 vmem[65536]; // 64K Forth memory

#define ptr(va)     ((void*)(vmem + (u16)(va)))
#define va(ptr)     ((u8*)(ptr) - vmem)

// cpu
void cpu_run(void);

i16 swi(i16 service, i16 arg[]);

// misc. lib functions
char *malloc_cstr(const void *adr, int len);

// console
int type(void *buf, int len);
int accept(void *buf, int size);

// disk i/o, functions return 0 if ok else errno
#define BLOCK_SIZE 1024
int disk_init(const char* blockfile);
int disk_read(int blk, void *addr);
int disk_write(int blk, void *addr);
int disk_flush(void);
